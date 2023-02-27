import DetailsBody from "./DetailsBody"
import DetailsFooter from "./DetailsFooter"
import DetailsHeader from "./DetailsHeader"

const Details = (props) => {
    
    return (
        <div className='conversation-details-wrap'>
            {<DetailsHeader visitor={props.visitor} />}
            {<DetailsBody visitor={props.visitor} />}
            {<DetailsFooter visitor={props.visitor} />}
        </div>
    )
}

export default Details