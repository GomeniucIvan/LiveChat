import React, { useEffect, useState } from 'react'
import $ from 'jquery';
import { NavLink, useLocation } from 'react-router-dom';
import { get } from '../../../utils/HttpClient';
import { isNullOrEmpty, KTSVG } from '../../../utils/Utils';

const SecondaryAside = () => {
    let [asideItems, setAsideItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const location = useLocation();

    useEffect(() => {
        const PopulateComponent = async () => {
            const response = await get(`SecondaryAsideList`, location);

            if (response && response.IsValid) {
                setAsideItems(response.Data);
            }
        }
        setLoading(false);
        PopulateComponent();
    }, []);

    return (
        <>
            {loading &&
                <></>
            }
            {!loading &&
                <div className='secondary-aside'>
                    <div className='secondary-aside-menu'>
                        <ul className='menu vertical'>
                            {asideItems.map(menuItem => {
                                return (
                                    <NavLink data-for={`${menuItem.SVGClass}-svg`} className={`secondary-link`} to={menuItem.Url} title={menuItem.Text} end>
                                        <KTSVG icon={menuItem.SVGClass} className='svg-icon-2' />
                                        <span>{menuItem.Text}</span>
                                    </NavLink>
                                )
                            })}
                        </ul>
                    </div>
                </div>
            }
        </>
    )
}

export { SecondaryAside }
