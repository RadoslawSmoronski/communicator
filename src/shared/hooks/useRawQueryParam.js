import { useLocation } from "react-router-dom";

const useRawQueryParam = (name) => {
  const location = useLocation();

  const search = location.search.substring(1);
  const pairs = search.split("&");

  for (const pair of pairs) {
    const [key, value] = pair.split("=");
    if (key === name) return value || null;
  }

  return null;
};

export default useRawQueryParam;
