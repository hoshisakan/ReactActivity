import { useField } from 'formik'
import { Form, Label } from 'semantic-ui-react'
import DatePicker, { ReactDatePickerProps } from 'react-datepicker'

export default function MyDateInput(props: Partial<ReactDatePickerProps>) {
    //TODO: Add exclamation mark in props.name to allow undefined.
    const [field, meta, helper] = useField(props.name!)
    return (
        <Form.Field error={meta.touched && !!meta.error}>
            <DatePicker
                {...field}
                {...props}
                selected={(field.value && new Date(field.value)) || null}
                onChange={(value) => helper.setValue(value)}
            />
            {meta.touched && meta.error ? (
                <Label basic color="red">
                    {meta.error}
                </Label>
            ) : null}
        </Form.Field>
    )
}
