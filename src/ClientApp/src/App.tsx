import { useCallback, useEffect, useState } from "react";
import { Context, getHome, Home, NotSignedInError } from "./api";
import { ToolFrame, ToolLaunch } from "./ToolFrame";

export function App() {
  const [home, setHome] = useState<Home>();
  const [error, setError] = useState<Error>();
  const [personId, setPersonId] = useState<number>();
  const [launch, setLaunch] = useState<ToolLaunch>();
  const [notice, setNotice] = useState<string>();

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
    (added: number) => {
      setLaunch(undefined);
      setNotice(`Added ${added} link${added === 1 ? "" : "s"}.`);
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
        <p className="notice" role="status">
          {notice} <button onClick={() => setNotice(undefined)}>Dismiss</button>
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

      {context.links.length === 0 ? (
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
                onClick={() =>
                  onLaunch({
                    title: `${link.title} (as ${personName})`,
                    url: link.launchUrls[personId],
                    deepLinking: false,
                  })
                }
              >
                Launch
              </button>
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
