import { Profile } from '../../../app/model/profile';
import ProfileCard from '../../profiles/ProfileCard';

import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import { List, Image, Popup } from 'semantic-ui-react';

interface Props {
    attendees: Profile[];
}

export default observer(function ActivityListItemAttendee({ attendees }: Props) {
    return (
        <List horizontal>
            {attendees.map((attendee) => (
                <>
                    <Popup
                        hoverable
                        key={attendee.username}
                        trigger={
                            <List.Item as={Link} to={`/profiles/${attendee.username}`}>
                                <Image size="mini" circular src={attendee.image || '/assets/user.png'} />
                            </List.Item>
                        }
                    >
                        <Popup.Content>
                            <ProfileCard profile={attendee} />
                        </Popup.Content>
                    </Popup>
                </>
            ))}
        </List>
    );
});
