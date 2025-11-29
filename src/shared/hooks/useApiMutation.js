import { useMutation } from "@tanstack/react-query";
import { useAxiosAuth } from "../../features/auth/hooks/useAxiosAuth";

export const useApiMutation = ({ url, method = "post" }) => {
  const api = useAxiosAuth();

  return useMutation({
    mutationFn: async (body) => {
      const res = await api[method](url, body);
      return res.data;
    },
  });
};
