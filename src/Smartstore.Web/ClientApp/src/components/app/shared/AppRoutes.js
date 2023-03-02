import Messages from '../../app/conversation/Messages'
import Widget from '../../launcher/Widget';
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
    }
];

export default AppRoutes;
