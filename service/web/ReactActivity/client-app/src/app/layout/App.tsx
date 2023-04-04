import NavBar from './NavBar'

import { Container } from 'semantic-ui-react'
import { observer } from 'mobx-react-lite'
import { Outlet, useLocation } from 'react-router-dom'
import HomePage from '../../features/home/HomePage'
import { Fragment } from 'react'

function App() {
    const location = useLocation()

    return (
        //TODO: Can remove the fragment use <></> instead, because render page result is the same.
        <Fragment>
            {location.pathname === '/' ? <HomePage /> : (
                <Fragment>
                    <NavBar />
                    <Container style={{ marginTop: '7em' }}>
                        <Outlet />
                    </Container>
                </Fragment>
            )}
        </Fragment>
    )
}

export default observer(App)
