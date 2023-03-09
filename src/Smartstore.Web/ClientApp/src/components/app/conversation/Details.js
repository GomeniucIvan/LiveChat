import DetailsBody from "./DetailsBody"
import DetailsFooter from "./DetailsFooter"
import DetailsHeader from "./DetailsHeader"

const Details = (props) => {
    return (
        <div className='conversation-details-wrap'>
            {<DetailsHeader message={props.message} />}
            {<DetailsBody message={props.message} />}
            {<DetailsFooter message={props.message} />}    
        </div>
    )
}

export default Details