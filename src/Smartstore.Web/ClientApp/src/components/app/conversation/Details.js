import { isNullOrEmpty } from "../../utils/Utils"
import DetailsBody from "./DetailsBody"
import DetailsFooter from "./DetailsFooter"
import DetailsHeader from "./DetailsHeader"

const Details = (props) => {
    return (
        <div className='conversation-details-wrap'>
            {isNullOrEmpty(props.visitorId) && 
                <>Select message</>
            }

            {!isNullOrEmpty(props.visitorId) &&
                <>
                    {<DetailsHeader visitorId={props.visitorId} />}
                    {<DetailsBody visitorId={props.visitorId} />}
                    {<DetailsFooter visitorId={props.visitorId} />}
                </>
            }     
        </div>
    )
}

export default Details