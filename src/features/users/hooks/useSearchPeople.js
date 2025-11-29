import { useState, useCallback } from "react";
import APIs from "../../../api/ApiURL";
import { useApi } from "../../../shared/hooks/useApi";

export const useSearchPeople = (userId) => {
  const { callApi } = useApi();

  const [peopleResult, setPeopleResult] = useState({
    list: [],
    findStatus: "not typed",
  });

  const searchPeople = useCallback(
    async (searchText) => {
      if (searchText.trim() === "") {
        setPeopleResult({
          list: [],
          findStatus: "not typed",
        });
        return;
      }

      const { data, error } = await callApi({
        url: APIs.FIND_PEOPLE_TO_INVITE(searchText, userId),
        method: "get",
        isAuth: true,
      });

      if (error) {
        if (error.response?.status === 404) {
          setPeopleResult({
            list: [],
            findStatus: "not found",
          });
        }
        return;
      }

      if (!data || data.length === 0) {
        setPeopleResult({
          list: [],
          findStatus: "not found",
        });
        return;
      }

      setPeopleResult({
        list: data,
        findStatus: "found",
      });
    },
    [userId, callApi]
  );

  return {
    peopleResult,
    searchPeople,
  };
};

export default useSearchPeople;
