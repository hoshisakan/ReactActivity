import React from 'react';
import { Button, Header, Segment } from 'semantic-ui-react';
import axios from 'axios';
import ValidationError from './ValidationError';


export default function TestErrors()
{
    const base = 'http://localhost:5001/api/'
    const [errors, setErrors] = React.useState(null);

    function handleNotFound()
    {
        axios.get(`${base}buggy/not-found`).catch(err => console.log(err.response));
    }

    function handleBadRequest()
    {
        axios.get(`${base}buggy/bad-request`).catch(err => console.log(err.response));
    }

    function handleServerError()
    {
        axios.get(`${base}buggy/server-error`).catch(err => console.log(err.response));
    }

    function handleUnauthorized()
    {
        axios.get(`${base}buggy/unauthorized`).catch(err => console.log(err.response));
    }

    function handleBadGuid()
    {
        axios.get(`${base}activities/not-a-guid`).catch(err => console.log(err.response));
    }

    function handleValidationError()
    {
        axios.post(`${base}activities`, {}).catch(err => setErrors(err));
    }

    return (
        <>
            <Header as='h1' content='Test Error component' />
            <Segment>
                <Button.Group widths='7'>
                    <Button onClick={handleNotFound} content='Not Found' basic primary />
                    <Button onClick={handleBadRequest} content='Bad Request' basic primary />
                    <Button onClick={handleValidationError} content='Validation Error' basic primary />
                    <Button onClick={handleServerError} content='Server Error' basic primary />
                    <Button onClick={handleUnauthorized} content='Unauthorized' basic primary />
                    <Button onClick={handleBadGuid} content='Bad Guid' basic primary />
                </Button.Group>
            </Segment>
            {errors && <ValidationError errors={errors} />}
        </>
    )
}