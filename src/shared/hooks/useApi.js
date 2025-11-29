import axios from "../../api/axios";
import { useAxiosAuth } from "../../features/auth/hooks/useAxiosAuth";

export const useApi = () => {
  const authAxios = useAxiosAuth();

  const callApi = async ({ url, method = "get", body = null, isAuth = false }) => {
    const client = isAuth ? authAxios : axios;

    try {
      const res = await client[method](url, body);
      return { data: res.data, error: null };
    } catch (error) {
      return { data: null, error };
    }
  };

  return { callApi };
};