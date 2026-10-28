import { useCallback, useEffect, useState } from "react";
import { Context, getHome, Home, NotSignedInError } from "./api";
import { DeepLinkingResult, ToolFrame, ToolLaunch } from "./ToolFrame";

export function App() {
  const [home, setHome] = useState<Home>();
  const [error, setError] = useState<Error>();
  const [personId, setPersonId] = useState<number>();
  const [launch, setLaunch] = useState<ToolLaunch>();
  const [notice, setNotice] = useState<{ text: string; error: boolean }>();

  const load = useCallback(() => {
    getHome()
      .then((h) => {
        setHome(h);
        setPersonId((current) => current ?? h.people[0]?.id);
      })
      .catch(setError);
  }, []);

  useEffect(load, [load]);

  const onDeepLinkingComplete = useCallback(
    ({ added, msg, errormsg }: DeepLinkingResult) => {
      setLaunch(undefined);
      // Show the tool's own msg / errormsg next to the count (Deep Linking 2.0 response).
      const text = [`Added ${added} item${added === 1 ? "" : "s"}.`, msg, errormsg].filter(Boolean).join(" ");
      setNotice({ text, error: !!errormsg });
      load();
    },
    [load],
  );

  if (error instanceof NotSignedInError) {
    return (
      <main>
        <h1>Advantage Platform</h1>
        <p>
          Please <a href="/Identity/Account/Login?ReturnUrl=%2Fapp%2F">sign in</a> first.
        </p>
      </main>
    );
  }
  if (error) return <main className="error">{error.message}</main>;
  if (!home || personId === undefined) return <main>Loading…</main>;

  const person = home.people.find((p) => p.id === personId);

  return (
    <main>
      <h1>Advantage Platform</h1>
      <p className="muted">
        The same sample platform as the <a href="/">Razor Pages UI</a>, with a React front end.
      </p>

      <label className="launch-as">
        Launch as{" "}
        <select value={personId} onChange={(e) => setPersonId(Number(e.target.value))}>
          {home.people.map((p) => (
            <option key={p.id} value={p.id}>
              {p.name} ({p.roles})
            </option>
          ))}
        </select>
      </label>

      {notice && (
        <p className={notice.error ? "notice error" : "notice"} role="status">
          {notice.text} <button onClick={() => setNotice(undefined)}>Dismiss</button>
        </p>
      )}

      <ContextSection
        heading="Platform links"
        context={home.platform}
        personId={personId}
        personName={person?.name}
        onLaunch={setLaunch}
      />
      <ContextSection
        heading={`Course: ${home.course.name}`}
        context={home.course}
        personId={personId}
        personName={person?.name}
        onLaunch={setLaunch}
      />

      {launch && (
        <ToolFrame
          launch={launch}
          onClose={() => setLaunch(undefined)}
          onDeepLinkingComplete={onDeepLinkingComplete}
        />
      )}
    </main>
  );
}

interface SectionProps {
  heading: string;
  context: Context;
  personId: number;
  personName?: string;
  onLaunch: (launch: ToolLaunch) => void;
}

function ContextSection({ heading, context, personId, personName, onLaunch }: SectionProps) {
  return (
    <section>
      <h2>{heading}</h2>

      {context.links.length === 0 && context.content.length === 0 ? (
        <p className="muted">No links yet.</p>
      ) : (
        <ul className="links">
          {context.links.map((link) => (
            <li key={link.id}>
              <div>
                <strong>{link.title}</strong>
                <div className="muted">
                  {link.toolName}
                  {link.description && ` · ${link.description}`}
                </div>
              </div>
              <button
                onClick={() => {
                  // Honour the presentation target the tool chose with Deep Linking.
                  if (link.documentTarget === "window") {
                    window.open(link.launchUrls[personId], "_blank", "noopener");
                    return;
                  }
                  onLaunch({
                    title: `${link.title} (as ${personName})`,
                    url: link.launchUrls[personId],
                    deepLinking: false,
                  });
                }}
              >
                {link.documentTarget === "window" ? "Launch in new tab" : "Launch"}
              </button>
            </li>
          ))}
        </ul>
      )}

      {context.content.length > 0 && (
        <ul className="links">
          {context.content.map((item) => (
            <li key={`content-${item.id}`}>
              <div>
                <strong>{item.title}</strong>
                <div className="muted">
                  {item.type}
                  {item.text && ` · ${item.text}`}
                </div>
              </div>
              {item.type === "image" ? (
                <img src={item.url} alt={item.title} style={{ maxHeight: "4em" }} />
              ) : (
                <a href={item.url} target="_blank" rel="noopener">
                  {item.type === "file" ? "Download" : "Open"}
                </a>
              )}
            </li>
          ))}
        </ul>
      )}

      {context.deepLinkTools.length > 0 && (
        <div className="add-content">
          Add content with:{" "}
          {context.deepLinkTools.map((tool) => (
            <button
              key={tool.id}
              onClick={() =>
                onLaunch({
                  title: `Add content from ${tool.name}`,
                  url: tool.launchUrls[personId],
                  deepLinking: true,
                })
              }
            >
              {tool.name}
            </button>
          ))}
        </div>
      )}
    </section>
  );
}
