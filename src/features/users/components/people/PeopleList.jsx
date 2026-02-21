import React, { useState, useContext, useEffect } from 'react'

import PersonTile from './PersonTile';
import Spinner from '../../../../shared/components/Spinner';
import { useSearchPeople } from '../../hooks/useSearchPeople';
import { UserContext } from '../../../../app/providers/UserProvider';

const PeopleList = ({ searchQuery }) => {
    const { user } = useContext(UserContext);

    const { searchPeople, loading, people } = useSearchPeople(user.userID);

    useEffect(() => {
        searchPeople(searchQuery);
    }, [searchQuery]);

    if (loading) return <Spinner />

    return (
        <>
            {
                searchQuery.trim() == "" ? (
                    <div className='infoText'>Please type any character</div>
                ) : people.length == 0 ? (
                    <div className='infoText'>There are no users named {searchQuery} ...</div>
                ) : (
                    people.map(u => (
                        <PersonTile
                            key={u.id}
                            username={u.username}
                            recipientId={u.id}
                            isInvited={u.isInvited}
                            avatarUrl={u.avatarUrl}
                        />
                    ))
                )
            }
        </>
    )
}

export default PeopleList