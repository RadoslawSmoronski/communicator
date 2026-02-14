import { useContext, useMemo, useEffect } from "react";
import { AuthContext } from "../../app/providers/AuthProvider";
import authAxios from "../../api/authAxios";
import mediaAxios from "../../api/mediaAxios";

// use
// api.get(URL, PARAMS, ISAUTH)
export const useApi = ({ mediaContent = false } = {}) => {
  const { accessToken, refreshAccessToken } = useContext(AuthContext);

  const client = useMemo(() => {
    const instance = mediaContent ? mediaAxios : authAxios;

    if (accessToken) {
      instance.defaults.headers.common.Authorization =
        `Bearer ${accessToken}`;
    }

    return instance;
  }, [accessToken]);

  // Auto refresh token if 401
  // exception for api: Change password
  useEffect(() => {

    const responseInterceptor = client.interceptors.response.use(
      (response) => response, // success
      async (error) => { // error
        const prevRequest = error?.config;

        const errorDetail = error?.response?.data?.detail;
        const status = error?.response?.status;

        // expired token error
        if (status === 401 && errorDetail == null && !prevRequest?.sent) {
          prevRequest.sent = true;

          try {
            console.log("Expired token detected - refreshing...");
            const newAccessToken = await refreshAccessToken();

            prevRequest.headers["Authorization"] = `Bearer ${newAccessToken}`;
            client.defaults.headers.common["Authorization"] = `Bearer ${newAccessToken}`;

            return client(prevRequest);
          } catch (refreshError) {
            return Promise.reject(refreshError);
          }
        }

        // other errors
        return Promise.reject(error);
      }
    );

    // clear inceptor
    return () => {
      client.interceptors.response.eject(responseInterceptor);
    };
  }, [client, refreshAccessToken]);

  // method for calling api
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
