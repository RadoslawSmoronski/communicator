import axios from "../../api/axios";
import { useAxiosAuth } from "../../features/auth/hooks/useAxiosAuth";

export const useApi = () => {
  const authAxios = useAxiosAuth();

  const request = async (client, method, url, data) => {
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

  const api = {
    get: (url, params = {}, isAuth = false) =>
      request(isAuth ? authAxios : axios, "get", url, params),

    post: (url, body = {}, isAuth = false) =>
      request(isAuth ? authAxios : axios, "post", url, body),

    put: (url, body = {}, isAuth = false) =>
      request(isAuth ? authAxios : axios, "put", url, body),

    patch: (url, body = {}, isAuth = false) =>
      request(isAuth ? authAxios : axios, "patch", url, body),

    delete: (url, body = {}, isAuth = false) =>
      request(isAuth ? authAxios : axios, "delete", url, body),
  };

  return api;
};
