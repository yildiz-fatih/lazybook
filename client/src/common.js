export const API = "http://localhost:5174/api";

// Token helpers
export function getToken() { return localStorage.getItem("access_token"); }
export function setToken(t) { localStorage.setItem("access_token", t); }
export function removeToken() { localStorage.removeItem("access_token"); }
