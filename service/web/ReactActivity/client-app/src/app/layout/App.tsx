import NavBar from './NavBar'
import ActivityDashboard from '../../features/activities/dashboard/ActivityDashboard'
import { useStore } from '../stores/store'
import LoadingComponent from './LoadingComponent'

import { useEffect } from 'react'
import { Container } from 'semantic-ui-react'
import { observer } from 'mobx-react-lite'

function App() {
    const { activityStore } = useStore()

    useEffect(() => {
        activityStore.loadActivities()
    }, [activityStore])

    if (activityStore.loadingInitial) {
        return <LoadingComponent content="Loading app" />
    }

    return (
        <>
            <NavBar />
            <Container style={{ marginTop: '10em' }}>
                <ActivityDashboard />
            </Container>
        </>
    )
}

export default observer(App)
