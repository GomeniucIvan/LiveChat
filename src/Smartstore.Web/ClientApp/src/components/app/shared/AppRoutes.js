import Index from '../../app/conversation/Index'
import LauncherWrapper from '../../launcher/LauncherWrapper';
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
        element: <LauncherWrapper />
    }
];

export default AppRoutes;
