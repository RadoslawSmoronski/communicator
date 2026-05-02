import cookieUtils from "../../../shared/utils/cookieUtils";

const COOKIE_NAME = 'lastOpenedChatSet';

export const lastOpenedChatService = {
    save: (data) => {
        cookieUtils.set(COOKIE_NAME, data);
    },

    load: () => {
        const data = cookieUtils.get(COOKIE_NAME);
        return data || {};
    },

    // remove by userId
    remove: (userId) => {
        const data = cookieUtils.get(COOKIE_NAME) || {};

        if (data[userId]) {
            delete data[userId];
            cookieUtils.set(COOKIE_NAME, data);
        }
    },

    clear: () => {
        cookieUtils.remove(COOKIE_NAME);
    }
};