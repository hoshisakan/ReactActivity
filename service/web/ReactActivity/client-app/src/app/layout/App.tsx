import { Activity } from '../model/activity'
import NavBar from './NavBar'
import ActivityDashboard from '../../features/activities/dashboard/ActivityDashboard'

import React, { Fragment, useEffect, useState } from 'react'
import axios from 'axios'
import { Container } from 'semantic-ui-react'
import { v4 as uuid } from 'uuid'

function App() {
    const [activities, setActivities] = useState<Activity[]>([])
    const [currSelectedActivity, setCurrSelectedActivity] = useState<Activity | undefined>(undefined)
    const [editMode, setEditMode] = useState(false)

    useEffect(() => {
        axios.get<Activity[]>('http://localhost:5001/api/activities').then((response) => {
            setActivities(response.data)
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
        activity.id
            ? setActivities([...activities.filter((x) => x.id !== activity.id), activity])
            : setActivities([...activities, {...activity, id: uuid() }])
        setEditMode(false)
        setCurrSelectedActivity(activity)
    }

    function handleDeleteActivity(id: string) {
        setActivities([...activities.filter((x) => x.id !== id)])
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
                />
            </Container>
        </>
    )
}

export default App
