import axios from "../../api/axios";

// use
// api.get(URL, PARAMS, ISAUTH)
export const useApi = (accessToken) => {
  const client = axios.create({
    withCredentials: true,
  });

  if (accessToken) {
    client.defaults.headers.common.Authorization =
      `Bearer ${accessToken}`;
  }

  const request = async (method, url, data) => {
    try {
      let response;

      if (method === "get") {
        response = await client.get(url, { params: data });
      } else if (method === "delete") {
        response = await client.delete(url, { data });
      } else {
        response = await client[method](url, data);
      }

      return { data: response.data, error: null };
    } catch (error) {
      return { data: null, error };
    }
  };

  return {
    get: (url, params) => request("get", url, params),
    post: (url, body) => request("post", url, body),
    put: (url, body) => request("put", url, body),
    patch: (url, body) => request("patch", url, body),
    delete: (url, body) => request("delete", url, body),
  };
};
