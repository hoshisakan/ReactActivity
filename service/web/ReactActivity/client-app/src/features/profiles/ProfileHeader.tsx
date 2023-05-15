import { Profile } from '../../app/models/profile';
import FollowButton from './FollowButton';
import { useStore } from '../../app/stores/store';

import { Divider, Grid, Header, Item, Segment, SemanticWIDTHS, Statistic } from 'semantic-ui-react';
import { observer } from 'mobx-react-lite';
import { useEffect } from 'react';

interface Props {
    profile: Profile;
}

export default observer(function ProfileHeader({ profile }: Props) {
    const { profileStore } = useStore();
    const { profileHeaderSizeLoaded, profileHeaderSize, setProfileHeaderComponentSize } = profileStore;

    useEffect(() => {
        if (!profileHeaderSizeLoaded) {
            setProfileHeaderComponentSize();
        }
    }, [profileHeaderSizeLoaded, profileStore, setProfileHeaderComponentSize]);

    return (
        <Segment>
            <Grid>
                <Grid.Column width={profileHeaderSize.contentWidth as SemanticWIDTHS}>
                    <Item.Group>
                        <Item>
                            <Item.Image avatar size="small" circular src={profile.image || '/assets/user.png'} />
                            <Item.Content verticalAlign="middle">
                                <Header as="h1" content={profile.displayName} />
                            </Item.Content>
                        </Item>
                    </Item.Group>
                </Grid.Column>
                <Grid.Column width={profileHeaderSize.featuresWidth as SemanticWIDTHS}>
                    <Statistic.Group widths={2}>
                        <Statistic label="Followers" value={profile.followersCount} />
                        <Statistic label="Following" value={profile.followingCount} />
                    </Statistic.Group>
                    <Divider />
                    <FollowButton profile={profile} />
                </Grid.Column>
            </Grid>
        </Segment>
    );
});
