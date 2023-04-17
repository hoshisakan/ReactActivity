import { Profile } from '../../app/models/profile';
import FollowButton from './FollowButton';

import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import { Card, Image } from 'semantic-ui-react';

interface Props {
    profile: Profile;
}

function truncate(sourceStr: string | undefined) {
    if (sourceStr) {
        return sourceStr.length > 50 ? sourceStr.substring(0, 50) + '...' : sourceStr;
    }
}

export default observer(function ProfileCard({ profile }: Props) {
    return (
        <Card as={Link} to={`/profiles/${profile.username}`}>
            <Image src={profile.image || '/assets/user.png'} />
            <Card.Content>
                <Card.Header>{profile.displayName}</Card.Header>
                <Card.Description>{truncate(profile.bio)}</Card.Description>
            </Card.Content>
            <Card.Content extra>
                <i className="user icon"></i>
                {profile.followersCount} followers
            </Card.Content>
            <FollowButton profile={profile} />
        </Card>
    );
});
