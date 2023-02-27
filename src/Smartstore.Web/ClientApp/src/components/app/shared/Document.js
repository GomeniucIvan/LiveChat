import { useEffect, useState} from 'react'
import { get } from "../../utils/HttpClient";
import { useLocation } from 'react-router-dom';
import { isNullOrEmpty, redirectToLogin, setHeaderData } from '../../utils/Utils';
import Login from '../identity/Login';
import Layout from './Layout';
import { Loading } from '../../utils/Loading';
import Install from './Install';
import LauncherWrapper from '../../launcher/LauncherWrapper';


const Document = () => {
    let [loading, setLoading] = useState(true);
    let [model, setModel] = useState(null);
    let [isRegistered, setIsRegistered] = useState(false);
    let [isInstalled, setIsInstalled] = useState(false);
    const location = useLocation();

    const isChatWidget = location.pathname.includes('/widget/');

    useEffect(() => {
        const PopulateComponent = async () => {
            const installResponse = await get(`Install`, location);

            if (installResponse.IsValid) {
                const installData = installResponse.Data;
                setIsInstalled(installData.IsInstalled);

                if (!installData.IsInstalled) {
                    redirectToLogin(/*response*/ null, /*forceRedirect*/ true);
                }
            } 
            if (installResponse.Data.IsInstalled) {
                const response = await get(`Startup`, location);
                const data = response.Data;

                if (response.IsValid) {
                    setModel(data);
                    setIsRegistered(data.IsRegistered);
                    setHeaderData(data);

                    if (!data.IsRegistered) {
                        redirectToLogin(/*response*/ null, /*forceRedirect*/ true);
                    }

                    if ((installResponse.Data.IsInstalled.IsInstalled && location.pathname.toLocaleLowerCase() === '/install') ||
                        (data.IsRegistered && location.pathname.toLocaleLowerCase() === '/login')) {
                        window.history.pushState({}, "", '/');
                    }

                    if (data.IsRegistered && !isNullOrEmpty(data.CompanyId)) {
                        localStorage.setItem('companyId', data.CompanyId);
                    }
                }
            }

            setLoading(false);
        }

        if (isChatWidget) {
            //todo count new messages
            //create guest, inser-update
        } else {
            PopulateComponent();
        }
    }, []);

    const onLogin = () => {
        let currentModel = model;
        currentModel.IsRegistered = true;
        setModel(currentModel);
        setIsRegistered(true);
        window.history.pushState({}, "", '/');
    }

    if (isChatWidget) {
        return <LauncherWrapper />
    }

    return (
        loading ? <Loading /> : !isInstalled ? <Install /> : (!isRegistered ? <Login onLogin={onLogin} /> : (<><Layout /></>))
    );
}

export default Document;
