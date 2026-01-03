import { useContext, useMemo } from "react";
import { AuthContext } from "../../app/providers/AuthProvider";
import authAxios from "../../api/authAxios";
import mediaAxios from "../../api/mediaAxios";

// use
// api.get(URL, PARAMS, ISAUTH)
export const useApi = ({ mediaContent = false } = {}) => {
  const { accessToken } = useContext(AuthContext);

  const client = useMemo(() => {
    const instance = mediaContent ? mediaAxios : authAxios;

    if (accessToken) {
      instance.defaults.headers.common.Authorization =
        `Bearer ${accessToken}`;
    }

    return instance;
  }, [accessToken]);

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

      return { data: response.data };
    } catch (error) {
      throw error;
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
