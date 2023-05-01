import { useStore } from '../stores/store';

import { Navigate, Outlet, useLocation } from 'react-router-dom';

export default function RequireAuth() {
    const { userStore, commonStore } = useStore();
    const { isLoggedIn } = userStore;
    const { isExistAccessToken, isExistRefreshToken } = commonStore;

    const location = useLocation();

    if (!isLoggedIn && !isExistAccessToken && !isExistRefreshToken) {
        return <Navigate to="/" state={{ from: location }} />;
    }
    return <Outlet />;
}
