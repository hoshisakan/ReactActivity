import { Activity } from '../model/activity'
import NavBar from './NavBar'
import ActivityDashboard from '../../features/activities/dashboard/ActivityDashboard'
import agent from '../api/agent'

import React, { Fragment, useEffect, useState } from 'react'
import { Container } from 'semantic-ui-react'
import { v4 as uuid } from 'uuid'
import LoadingComponent from './LoadingComponent'

function App() {
    const [activities, setActivities] = useState<Activity[]>([])
    const [currSelectedActivity, setCurrSelectedActivity] = useState<Activity | undefined>(undefined)
    const [editMode, setEditMode] = useState(false)
    const [loading, setLoading] = useState(true)
    const [submitting, setSubmitting] = useState(false)

    useEffect(() => {
        agent.Activities.list().then((response) => {
            let activities: Activity[] = []
            response.forEach(activity => {
                activity.date = activity.date.split('T')[0]
                activities.push(activity)
            })
            setActivities(activities)
            setLoading(false)
        })
    }, [])

    function handleSelectActivity(id: string) {
        setCurrSelectedActivity(activities.find((x) => x.id === id))
    }

    function handleCancelSelectActivity() {
        setCurrSelectedActivity(undefined)
    }

    function handleFormOpen(id?: string) {
        id ? handleSelectActivity(id) : handleCancelSelectActivity()
        setEditMode(true)
    }

    function handleFormClose() {
        setEditMode(false)
    }

    function handleCreateOrEditActivity(activity: Activity) {
        setSubmitting(true)
        // TODO if activity id exists, then update it, otherwise create a new activity
        if (activity.id)
        {
            agent.Activities.update(activity).then(() => {
                setActivities([...activities.filter((x) => x.id !== activity.id), activity])
                setCurrSelectedActivity(activity)
                setEditMode(false)
                setSubmitting(false)
            })
        }
        else {
            activity.id = uuid();
            agent.Activities.create(activity).then(() => {
                setActivities([...activities, activity])
                setCurrSelectedActivity(activity)
                setEditMode(false)
                setSubmitting(false)
            })
        }
    }

    function handleDeleteActivity(id: string) {
        setSubmitting(true)
        agent.Activities.delete(id).then(() => {
            //TODO remove activity with the given id from the activities array
            setActivities([...activities.filter((x) => x.id !== id)])
            setSubmitting(false)
        })
    }

    if (loading) {
        return <LoadingComponent content='Loading app' />
    }

    return (
        <>
            <NavBar openForm={handleFormOpen} />
            <Container style={{ marginTop: '10em' }}>
                {/* TODO Add ActivityDashboard component, pass in activities, currSelectedActivity, selectActivity, and cancelSelectActivity */}
                <ActivityDashboard
                    activities={activities}
                    currSelectedActivity={currSelectedActivity}
                    selectActivity={handleSelectActivity}
                    cancelSelectActivity={handleCancelSelectActivity}
                    editMode={editMode}
                    openForm={handleFormOpen}
                    closeForm={handleFormClose}
                    createOrEdit={handleCreateOrEditActivity}
                    deleteActivity={handleDeleteActivity}
                    submitting={submitting}
                />
            </Container>
        </>
    )
}

export default App
