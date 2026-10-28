// Shapes returned by GET /api/spa/home (see Spa/SpaApi.cs).

export interface Person {
  id: number;
  name: string;
  roles: string;
}

/** Launch URLs per person id. They point at the platform's /OidcLaunch page. */
export type LaunchUrls = Record<number, string>;

export interface Link {
  id: number;
  title: string;
  description: string | null;
  toolName: string;
  launchUrls: LaunchUrls;
}

export interface DeepLinkTool {
  id: number;
  name: string;
  launchUrls: LaunchUrls;
}

export interface Context {
  name: string;
  links: Link[];
  deepLinkTools: DeepLinkTool[];
}

export interface Home {
  platform: Context;
  course: Context;
  people: Person[];
}

export class NotSignedInError extends Error {}

export async function getHome(): Promise<Home> {
  // Same origin, so the browser sends the Identity cookie. No tokens in JavaScript.
  const response = await fetch("/api/spa/home", { credentials: "same-origin" });
  if (response.status === 401) {
    throw new NotSignedInError();
  }
  if (!response.ok) {
    throw new Error(`GET /api/spa/home failed: ${response.status}`);
  }
  return response.json();
}
