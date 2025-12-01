import { useContext, useEffect } from "react";
import { AuthContext } from "../../../app/providers/AuthProvider";
import authAxios from "../../../api/authAxios";
import { tokenService } from "../services/tokenService";

// add token to axios
export const useAxiosAuth = () => {
  const { accessToken, refreshAccessToken } = useContext(AuthContext);

  useEffect(() => {
    // === REQUEST INTERCEPTOR ===
    const reqInterceptor = authAxios.interceptors.request.use(
      (config) => {
        if (accessToken && !tokenService.isExpired(accessToken)) {
          config.headers.Authorization = `Bearer ${accessToken}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // === RESPONSE INTERCEPTOR (401 → refresh → retry) ===
    const resInterceptor = authAxios.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalReq = error.config;

        if (error.response?.status === 401 && !originalReq._retry) {
          originalReq._retry = true;

          // await refreshAccessToken();
          return authAxios(originalReq);
        }

        return Promise.reject(error);
      }
    );

    return () => {
      authAxios.interceptors.request.eject(reqInterceptor);
      authAxios.interceptors.response.eject(resInterceptor);
    };
  }, [accessToken]);

  return authAxios;
};
