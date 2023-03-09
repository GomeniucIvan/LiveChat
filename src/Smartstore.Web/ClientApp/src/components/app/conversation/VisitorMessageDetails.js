import { useEffect, useState } from "react";
import { equal, KTSVG } from "../../utils/Utils";
import Details from "./Details"

const VisitorMessageDetails = (props) => {
    const [showVisitorDetails, setShowVisitorDetails] = useState(false);

    useEffect(() => {
        setShowVisitorDetails(localStorage.getItem('showVisitorDetails'));
    })

    function expandOrCollapseVisitorDetails() {
        if (equal(showVisitorDetails, true)) {
            localStorage.setItem('showVisitorDetails', false);
            setShowVisitorDetails(false);
        } else {
            localStorage.setItem('showVisitorDetails', true);
            setShowVisitorDetails(true);
        }
    }

    return (
        <>
            {<Details message={props.message} />}


            <div className='conversation-details-summary-wrapper'>
                <div onClick={() => expandOrCollapseVisitorDetails()}>
                    <KTSVG width='30' height='30' icon={`${equal(showVisitorDetails, true) ? 'arrow-chevron-right-outline' : 'arrow-chevron-left-outline'}`} className={`conversation-details-summary-expander`} />
                </div>

                <div className={`conversation-details-summary`} style={{ display: equal(showVisitorDetails, true) ? 'block' : 'none' }}>

                </div>
            </div>
            
        </>
    )
}

export default VisitorMessageDetails