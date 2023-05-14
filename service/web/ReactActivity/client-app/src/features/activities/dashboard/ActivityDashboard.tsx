import { useStore } from '../../../app/stores/store';
import ActivityList from './ActivityList';
import ActivityFilters from './ActivityFilters';
import { PagingParams } from '../../../app/models/pagination';
import ActivityListItemPlaceholder from './ActivityListItemPlaceholder';

import { Grid, Loader, SemanticWIDTHS } from 'semantic-ui-react';
import { observer } from 'mobx-react-lite';
import { useEffect, useState } from 'react';
import InfiniteScroll from 'react-infinite-scroller';

export default observer(function ActivityDashboard() {
    const { activityStore, commonStore } = useStore();
    const {
        loadActivities,
        activityRegistry,
        setPagingParams,
        pagination,
        dashBoardSizeLoaded,
        setActivityDashboardComponentSize,
        activityDashboardSize,
    } = activityStore;
    const { detectedMobileDevice } = commonStore;
    const [loadingNext, setLoadingNext] = useState(false);

    function handleGetNext() {
        setLoadingNext(true);
        setPagingParams(new PagingParams(pagination!.currentPage + 1));
        loadActivities().then(() => setLoadingNext(false));
    }

    useEffect(() => {
        if (activityRegistry.size <= 1) {
            loadActivities();
        }
        if (!dashBoardSizeLoaded) {
            setActivityDashboardComponentSize();
        }
    }, [loadActivities, activityRegistry.size, dashBoardSizeLoaded, setActivityDashboardComponentSize]);

    return (
        <>
            {detectedMobileDevice ? (
                <Grid>
                    <Grid.Column width={activityDashboardSize.activityFiltersSidebarWidth as SemanticWIDTHS}>
                        <ActivityFilters />
                    </Grid.Column>
                    <Grid.Column width={activityDashboardSize.activityListItemWidth as SemanticWIDTHS}>
                        {activityStore.loadingInitial && !loadingNext ? (
                            <>
                                <ActivityListItemPlaceholder />
                                <ActivityListItemPlaceholder />
                            </>
                        ) : (
                            <InfiniteScroll
                                pageStart={0}
                                loadMore={handleGetNext}
                                hasMore={!loadingNext && !!pagination && pagination.currentPage < pagination.totalPages}
                                initialLoad={false}
                            >
                                <ActivityList />
                            </InfiniteScroll>
                        )}
                    </Grid.Column>
                    <Grid.Column width={10}>
                        <Loader active={loadingNext} />
                    </Grid.Column>
                </Grid>
            ) : (
                <Grid>
                    <Grid.Column width={activityDashboardSize.activityListItemWidth as SemanticWIDTHS}>
                        {activityStore.loadingInitial && !loadingNext ? (
                            <>
                                <ActivityListItemPlaceholder />
                                <ActivityListItemPlaceholder />
                            </>
                        ) : (
                            <InfiniteScroll
                                pageStart={0}
                                loadMore={handleGetNext}
                                hasMore={!loadingNext && !!pagination && pagination.currentPage < pagination.totalPages}
                                initialLoad={false}
                            >
                                <ActivityList />
                            </InfiniteScroll>
                        )}
                    </Grid.Column>
                    <Grid.Column width={activityDashboardSize.activityFiltersSidebarWidth as SemanticWIDTHS}>
                        <ActivityFilters />
                    </Grid.Column>
                    <Grid.Column width={10}>
                        <Loader active={loadingNext} />
                    </Grid.Column>
                </Grid>
            )}
        </>
    );
});
