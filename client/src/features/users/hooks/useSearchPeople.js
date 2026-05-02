import { useState, useTransition, useContext } from "react";
import { useApi } from "../../../shared/hooks/useApi";

import { mapPeopleList } from "../mappers/peopleMapper";
import APIs from "../../../api/ApiURL";

import { usersDtoMock } from "../mocks/getPeopleByTextMock";

export const useSearchPeople = (userId, useMock = false) => {
  const api = useApi();

  const [loading, startTransition] = useTransition();

  const [people, setPeople] = useState([]);

  const clear = () => {
    setPeople([]);
  };

  const searchPeople = async (searchText) => {
    if (!searchText || !searchText.trim()) {
      clear();
      return;
    }


    // API or Mock
    const fetchPeople = async () => {
      if (useMock) {
        return { data: usersDtoMock };
      } else {
        return await api.get(APIs.FIND_PEOPLE_TO_INVITE(searchText, userId));
      }
    };

    try {
      const { data, error } = await fetchPeople();

      startTransition(() => {
        setPeople(mapPeopleList(data));
      });
    } catch (err) {
      setPeople([]);
    }
  };

  return {
    people,
    loading,
    searchPeople,
    clear,
  };
};
