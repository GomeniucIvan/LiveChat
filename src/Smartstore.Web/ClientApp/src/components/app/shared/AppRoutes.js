import Index from '../../app/conversation/Index'
import Widget from '../../launcher/Widget';
import Install from './Install';

const AppRoutes = [
    {
        index: true,
        path: '/',
        element: <Index />
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
