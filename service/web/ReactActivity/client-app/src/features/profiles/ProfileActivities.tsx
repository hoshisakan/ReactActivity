import { useStore } from '../../app/stores/store';
import { UserActivity } from '../../app/models/profile';
import { dropDownOptions, tabPanes } from '../../app/common/options/activityEventOptions';

import { observer } from 'mobx-react-lite';
import { useEffect, SyntheticEvent } from 'react';
import { Link } from 'react-router-dom';
import { Card, Grid, Header, Tab, TabProps, Image, SemanticWIDTHS, Dropdown } from 'semantic-ui-react';
import { format } from 'date-fns';

export default observer(function ProfileActivities() {
    const { profileStore, commonStore } = useStore();
    const {
        loadUserActivities,
        profile,
        loadingActivities,
        userActivities,
        profileContentEventsSizeLoaded,
        profileContentEventsSize,
        setProfileContentEventsComponentSize,
    } = profileStore;
    const { detectedMobileDevice } = commonStore;

    useEffect(() => {
        loadUserActivities(profile!.username);
        if (!profileContentEventsSizeLoaded) {
            setProfileContentEventsComponentSize();
        }
    }, [loadUserActivities, profile, profileContentEventsSizeLoaded, setProfileContentEventsComponentSize]);

    const handleTabChange = (e: SyntheticEvent, data: TabProps) => {
        loadUserActivities(profile!.username, tabPanes[data.activeIndex as number].pane.key);
    };

    const handleDropDownChange = (e: SyntheticEvent, data: any) => {
        loadUserActivities(profile!.username, data.value);
    };

    return (
        <Tab.Pane loading={loadingActivities}>
            <Grid>
                <Grid.Column width={16}>
                    <Header floated="left" icon="calendar" content={'Activities'} />
                </Grid.Column>
                <Grid.Column width={16}>
                    {!detectedMobileDevice ? (
                        <Tab
                            panes={tabPanes}
                            menu={{ secondary: true, pointing: true }}
                            onTabChange={(e, data) => handleTabChange(e, data)}
                        />
                    ) : (
                        <Dropdown
                            placeholder="Please select"
                            fluid
                            selection
                            options={dropDownOptions}
                            onChange={handleDropDownChange}
                        />
                    )}
                    <br />
                    <Card.Group itemsPerRow={profileContentEventsSize.cardGroupItemsPerRow as SemanticWIDTHS}>
                        {userActivities.map((activity: UserActivity) => (
                            <Card as={Link} to={`/activities/${activity.id}`} key={activity.id}>
                                <Image
                                    src={`/assets/categoryImages/${activity.category}.jpg`}
                                    style={{ minHeight: 100, objectFit: 'cover' }}
                                />
                                <Card.Content>
                                    <Card.Header textAlign="center">{activity.title}</Card.Header>
                                    <Card.Meta textAlign="center">
                                        <div>{format(new Date(activity.date), 'do LLL')}</div>
                                        <div>{format(new Date(activity.date), 'h:mm a')}</div>
                                    </Card.Meta>
                                </Card.Content>
                            </Card>
                        ))}
                    </Card.Group>
                </Grid.Column>
            </Grid>
        </Tab.Pane>
    );
});
