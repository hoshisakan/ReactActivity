import NavBar from './NavBar';
import HomePage from '../../features/home/HomePage';
import { useStore } from '../stores/store';
import LoadingComponent from './LoadingComponent';
import ModalContainer from '../common/modals/ModalContainer';
import NavBarForMobile from './NavBarForMobile';

import { Container } from 'semantic-ui-react';
import { observer } from 'mobx-react-lite';
import { Outlet, useLocation } from 'react-router-dom';
import { Fragment, useEffect } from 'react';
import { ToastContainer } from 'react-toastify';

function App() {
    const location = useLocation();
    const { commonStore, userStore } = useStore();
    const { detectedMobileDevice } = commonStore;

    useEffect(() => {
        if (commonStore.token) {
            userStore.getUser().finally(() => commonStore.setAppLoaded());
            commonStore.setDetectedMobileDevice();
        }
        //TODO: User not logged in
        else {
            commonStore.setAppLoaded();
        }
    }, [commonStore, userStore]);

    if (!commonStore.appLoaded) {
        return <LoadingComponent content="Loading app..." />;
    }

    return (
        //TODO: Can remove the fragment use <></> instead, because render page result is the same.
        <Fragment>
            <ModalContainer />
            <ToastContainer position="bottom-right" hideProgressBar theme="colored" />
            {location.pathname === '/' ? (
                <HomePage />
            ) : (
                <Fragment>
                    {detectedMobileDevice ? <NavBarForMobile /> : <NavBar />}
                    <Container style={{ marginTop: '7em' }}>
                        <Outlet />
                    </Container>
                </Fragment>
            )}
        </Fragment>
    );
}

export default observer(App);
