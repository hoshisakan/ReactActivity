import { useStore } from '../stores/store';

import { Link, NavLink } from 'react-router-dom';
import { Container, Menu, Image, Dropdown } from 'semantic-ui-react';
import { observer } from 'mobx-react-lite';

export default observer(function NavBarForMobile() {
    const {
        userStore: { user, logout, isLoggedIn },
    } = useStore();
    return (
        <Menu size="small" inverted fixed="top">
            <Container>
                <Menu.Item as={NavLink} to="/" header>
                    <img src="/assets/logo.png" alt="logo" style={{ marginRight: '10px' }} />
                    Reactivities
                </Menu.Item>
                {isLoggedIn && (
                    <>
                        <Menu.Item as={NavLink} to="/activities" name="Activities" />
                        {/* <Menu.Item as={NavLink} to="/errors" name="Errors" /> */}
                        <Menu.Item position="right">
                            <Image
                                as={Link}
                                to={`/profiles/${user?.username}`}
                                src={user?.image || '/assets/user.png'}
                                avatar
                            />
                        </Menu.Item>
                        <Menu.Menu position="right">
                            <Dropdown item text={user?.displayName}>
                                <Dropdown.Menu>
                                    <Dropdown.Item
                                        as={Link}
                                        to="/createActivity"
                                        text="Create Activity"
                                        icon="building"
                                    />
                                    <Dropdown.Item
                                        as={Link}
                                        to={`/profiles/${user?.username}`}
                                        text="My Profile"
                                        icon="user"
                                    />
                                    <Dropdown.Item onClick={logout} text="Logout" icon="power" />
                                </Dropdown.Menu>
                            </Dropdown>
                        </Menu.Menu>
                    </>
                )}
            </Container>
        </Menu>
    );
});
