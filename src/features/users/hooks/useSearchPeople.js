import { useState, useTransition, useContext } from "react";
import { useApi } from "../../../shared/hooks/useApi";
import { AuthContext } from "../../../app/providers/AuthProvider";

import { mapPeopleList } from "../mappers/peopleMapper";
import APIs from "../../../api/ApiURL";

import { mockUsersDtos } from "../mocks/getPeopleByTextMock";

export const useSearchPeople = (userId, useMock = false) => {
  const { accessToken } = useContext(AuthContext);
  const api = useApi(accessToken);

  const [isPending, startTransition] = useTransition();

  const [people, setPeople] = useState([]);
  const [status, setStatus] = useState("idle");
  // idle | typing | found | not-found | error

  const clear = () => {
    setPeople([]);
    setStatus("idle");
  };

  const searchPeople = async (searchText) => {
    if (!searchText || !searchText.trim()) {
      clear();
      return;
    }

    setStatus("typing");

    // API or Mock
    const fetchPeople = async () => {
      if (useMock) {
        return { data: mockUsersDtos };
      } else {
        return await api.get(APIs.FIND_PEOPLE_TO_INVITE(searchText, userId));
      }
    };

    try {
      const { data, error } = await fetchPeople();

      startTransition(() => {
        if (error) {
          if (error.response?.status === 400) {
            setStatus("idle");
            setPeople([]);
            return;
          }

          if (error.response?.status === 404) {
            setStatus("not-found");
            setPeople([]);
            return;
          }

          setStatus("error");
          setPeople([]);
          return;
        }

        if (!data?.length) {
          setStatus("not-found");
          setPeople([]);
          return;
        }

        setPeople(mapPeopleList(data));
        setStatus("found");
      });
    } catch (err) {
      setStatus("error");
      setPeople([]);
    }
  };

  return {
    people,
    status,
    isPending, // for spinner (isLoading)
    searchPeople,
    clear,
  };
};
