window.todoStorage = window.todoStorage || {
    get: (key) => {
        try {
            return window.localStorage.getItem(key);
        } catch (error) {
            console.error('todoStorage.get failed', error);
            return null;
        }
    },
    set: (key, value) => {
        try {
            window.localStorage.setItem(key, value);
        } catch (error) {
            console.error('todoStorage.set failed', error);
            throw error;
        }
    },
    remove: (key) => {
        try {
            window.localStorage.removeItem(key);
        } catch (error) {
            console.error('todoStorage.remove failed', error);
        }
    }
};
