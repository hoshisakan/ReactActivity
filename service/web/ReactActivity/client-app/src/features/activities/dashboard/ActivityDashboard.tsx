import { Activity } from '../../../app/model/activity'
import ActivityDetails from '../details/ActivityDetails'

import React from 'react'
import { Grid } from 'semantic-ui-react'
import ActivityList from './ActivityList'
import ActivityForm from '../form/ActivityForm'

interface Props {
    activities: Activity[]
    currSelectedActivity: Activity | undefined
    selectActivity: (id: string) => void
    cancelSelectActivity: () => void
    editMode: boolean
    openForm: (id: string) => void
    closeForm: () => void
    createOrEdit: (activity: Activity) => void
    deleteActivity: (id: string) => void
    submitting: boolean
}

export default function ActivityDashboard({
    activities,
    currSelectedActivity,
    selectActivity,
    cancelSelectActivity,
    editMode,
    openForm,
    closeForm,
    createOrEdit,
    deleteActivity,
    submitting,
}: Props) {
    return (
        <Grid>
            <Grid.Column width={10}>
                {/* TODO: Add ActivityList component, pass in activities and selectActivity */}
                <ActivityList
                    activities={activities}
                    selectActivity={selectActivity}
                    deleteActivity={deleteActivity}
                    submitting={submitting}
                />
            </Grid.Column>
            <Grid.Column width={6}>
                {currSelectedActivity && !editMode && (
                    <ActivityDetails
                        activity={currSelectedActivity}
                        cancelSelectActivity={cancelSelectActivity}
                        openForm={openForm}
                    />
                )}
                {editMode && (
                    <ActivityForm
                        activity={currSelectedActivity}
                        closeForm={closeForm}
                        createOrEdit={createOrEdit}
                        submitting={submitting}
                    />
                )}
            </Grid.Column>
        </Grid>
    )
}
