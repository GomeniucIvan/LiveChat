import DetailsBody from "./DetailsBody"
import DetailsFooter from "./DetailsFooter"
import DetailsHeader from "./DetailsHeader"

const Details = (props) => {
    return (
        <div className='conversation-details-wrap'>
            {<DetailsHeader visitorId={props.visitorId} />}
            {<DetailsBody visitorId={props.visitorId} />}
            {<DetailsFooter visitorId={props.visitorId} />}    
        </div>
    )
}

export default Details