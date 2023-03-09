const DetailsHeader = (props) => {
    return (
        <>
            <div className='conv-header'>
                <h5 className={`margin-0`}>{props.message.FullName }</h5>
            </div>
        </>
    )
}

export default DetailsHeader