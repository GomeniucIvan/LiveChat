import Messages from '../../app/conversation/Messages'
import Widget from '../../launcher/Widget';
import VisitorMessageDetails from '../conversation/VisitorMessageDetails';
import { Account } from '../settings/Account';
import Install from './Install';

const AppRoutes = [
    {
        index: true,
        path: '/',
        element: <Messages />
    },
    {
        path: '/install',
        element: <Install />
    },
    {
        path: '/widget',
        element: <Widget />
    },
    {
        path: '/message/:urlVisitorId',
        element: <Messages />
    },
    {
        path: '/settings/general',
        element: <Account />
    }
];

export default AppRoutes;
