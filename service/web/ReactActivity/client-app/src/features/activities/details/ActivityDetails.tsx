import LoadingComponent from '../../../app/layout/LoadingComponent';
import { useStore } from '../../../app/stores/store';
import ActivityDetailedHeader from './ActivityDetailedHeader';
import ActivityDetailedInfo from './ActivityDetailedInfo';
import ActivityDetailedChat from './ActivityDetailedChat';
import ActivityDetailedSidebar from './ActivityDetailedSidebar';

import { Grid, SemanticWIDTHS } from 'semantic-ui-react';
import { useParams } from 'react-router-dom';
import { useEffect } from 'react';
import { observer } from 'mobx-react-lite';

export default observer(function ActivityList() {
    const { activityStore, commonStore } = useStore();
    const {
        currSelectedActivity: activity,
        loadActivity,
        loadingInitial,
        clearSelectedActivity,
        detailsSizeLoaded,
        setActivityDetailsComponentSize,
        activityDetailsSize,
    } = activityStore;
    const { detectedMobileDevice } = commonStore;
    const { id } = useParams();

    useEffect(() => {
        if (id) {
            loadActivity(id);
        }
        if (!detailsSizeLoaded) {
            setActivityDetailsComponentSize();
        }
        return () => clearSelectedActivity();
    }, [id, loadActivity, clearSelectedActivity, detailsSizeLoaded, setActivityDetailsComponentSize]);

    if (loadingInitial || !activity) {
        return <LoadingComponent />;
    }

    return (
        <>
            {detectedMobileDevice ? (
                <Grid>
                    <Grid.Column width={activityDetailsSize.activityDetailedSidebarWidth as SemanticWIDTHS}>
                        <ActivityDetailedSidebar activity={activity} />
                    </Grid.Column>
                    <Grid.Column width={activityDetailsSize.activityDetailedCardWidth as SemanticWIDTHS}>
                        <ActivityDetailedHeader activity={activity} />
                        <ActivityDetailedInfo activity={activity} />
                        <ActivityDetailedChat activityId={activity.id} />
                    </Grid.Column>
                </Grid>
            ) : (
                <Grid>
                    <Grid.Column width={activityDetailsSize.activityDetailedCardWidth as SemanticWIDTHS}>
                        <ActivityDetailedHeader activity={activity} />
                        <ActivityDetailedInfo activity={activity} />
                        <ActivityDetailedChat activityId={activity.id} />
                    </Grid.Column>
                    <Grid.Column width={activityDetailsSize.activityDetailedSidebarWidth as SemanticWIDTHS}>
                        <ActivityDetailedSidebar activity={activity} />
                    </Grid.Column>
                </Grid>
            )}
        </>
    );
});
