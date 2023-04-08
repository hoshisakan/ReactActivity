import NavBar from './NavBar';
import HomePage from '../../features/home/HomePage';
import { useStore } from '../stores/store';

import { Container } from 'semantic-ui-react';
import { observer } from 'mobx-react-lite';
import { Outlet, useLocation } from 'react-router-dom';
import { Fragment, useEffect } from 'react';
import { ToastContainer } from 'react-toastify';
import LoadingComponent from './LoadingComponent';
import ModalContainer from '../common/modals/ModalContainer';


function App() {
    const location = useLocation();
    const {commonStore, userStore} = useStore();

    useEffect(() => {
        if (commonStore.token) {
            userStore.getUser().finally(() => commonStore.setAppLoaded())
        }
        else {
            commonStore.setAppLoaded();
        }
    }, [commonStore, userStore])

    if (!commonStore.appLoaded)
    {
        return <LoadingComponent content='Loading app...' />
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
                    <NavBar />
                    <Container style={{ marginTop: '7em' }}>
                        <Outlet />
                    </Container>
                </Fragment>
            )}
        </Fragment>
    );
}

export default observer(App);
