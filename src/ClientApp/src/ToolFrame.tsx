import { useEffect } from "react";

// Origins that /DeepLinks may post its "done" message from: the platform itself, plus the
// platform's address while React runs on the Vite dev server.
const platformOrigins = [window.location.origin, import.meta.env.VITE_PLATFORM_ORIGIN].filter(Boolean);

export interface ToolLaunch {
  title: string;
  url: string;
  deepLinking: boolean;
}

/** What /DeepLinks posts when the tool is done: msg and errormsg come from the tool. */
export interface DeepLinkingResult {
  added: number;
  msg?: string | null;
  errormsg?: string | null;
}

interface Props {
  launch: ToolLaunch;
  onClose: () => void;
  onDeepLinkingComplete: (result: DeepLinkingResult) => void;
}

/**
 * Shows a tool in an iframe.
 *
 * Setting the iframe's src to the launch URL is all React does. From there the browser runs
 * the LTI 1.3 flow inside the iframe, without React:
 *   /OidcLaunch → tool's login URL → platform /connect/authorize → form POST of the signed
 *   id_token to the tool's launch URL.
 * For deep linking, the tool finally POSTs its response to the platform's /DeepLinks page,
 * which saves the links and posts a message to this window.
 */
export function ToolFrame({ launch, onClose, onDeepLinkingComplete }: Props) {
  useEffect(() => {
    if (!launch.deepLinking) return;

    function onMessage(event: MessageEvent) {
      // Ignore messages from the tool or anyone else: only the platform's /DeepLinks page counts.
      if (!platformOrigins.includes(event.origin)) return;
      if (event.data?.type !== "lti-deep-linking-complete") return;
      onDeepLinkingComplete({
        added: Number(event.data.added) || 0,
        msg: event.data.msg,
        errormsg: event.data.errormsg,
      });
    }

    window.addEventListener("message", onMessage);
    return () => window.removeEventListener("message", onMessage);
  }, [launch, onDeepLinkingComplete]);

  return (
    <div className="frame-backdrop" role="dialog" aria-modal="true" aria-label={launch.title}>
      <div className="frame-panel">
        <header>
          <strong>{launch.title}</strong>
          <span className="spacer" />
          {!launch.deepLinking && (
            // A new tab is the fallback when a tool refuses to run in an iframe, for
            // example because its session cookie is blocked as a third-party cookie.
            <a href={launch.url} target="_blank" rel="noopener">
              Open in new tab
            </a>
          )}
          <button onClick={onClose}>Close</button>
        </header>
        <iframe
          // A new key per launch makes React create a fresh iframe, so every launch
          // starts a new LTI flow.
          key={launch.url}
          src={launch.url}
          title={launch.title}
          allow="fullscreen; clipboard-write"
        />
      </div>
    </div>
  );
}
