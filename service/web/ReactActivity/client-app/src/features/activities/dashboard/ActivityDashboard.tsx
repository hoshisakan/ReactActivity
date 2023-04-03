import ActivityDetails from '../details/ActivityDetails'
import { useStore } from '../../../app/stores/store'
import ActivityList from './ActivityList'
import ActivityForm from '../form/ActivityForm'

import { Grid } from 'semantic-ui-react'
import { observer } from 'mobx-react-lite'

export default observer(function ActivityDashboard() {
    const { activityStore } = useStore()
    const { currSelectedActivity, editMode } = activityStore

    return (
        <Grid>
            <Grid.Column width={10}>
                <ActivityList />
            </Grid.Column>
            <Grid.Column width={6}>
                {currSelectedActivity && !editMode && <ActivityDetails />}
                {editMode && <ActivityForm />}
            </Grid.Column>
        </Grid>
    )
})
