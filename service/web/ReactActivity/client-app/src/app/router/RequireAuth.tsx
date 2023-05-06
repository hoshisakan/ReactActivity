import { useStore } from '../stores/store';

import { Navigate, Outlet, useLocation } from 'react-router-dom';

export default function RequireAuth() {
    const { userStore } = useStore();
    const { isLoggedIn } = userStore;

    const location = useLocation();

    if (!isLoggedIn) {
        return <Navigate to="/" state={{ from: location }} />;
    }
    return <Outlet />;
}
