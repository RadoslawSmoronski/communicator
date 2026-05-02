export const userDataService = {
    save: (userData) => {
        sessionStorage.setItem('userData', JSON.stringify(userData));
    },

    load: () => (
        sessionStorage.getItem("userData")
    ),

    clear: () => {
        sessionStorage.removeItem("userData");
    },
}