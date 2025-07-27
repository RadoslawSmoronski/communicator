import React from 'react'

const Avatar = ({ url, size = 50 }) => {

    return (
        url != null ?
            (
                <div className="friendTileIcon"
                    style={{
                        backgroundImage: `url(${url})`,
                        width: `${size}px`,
                        height: `${size}px`,
                        borderRadius: `${size / 2}px`,
                    }}
                />
            )
            :
            (<div className="friendTileIcon"
                style={{
                    width: `${size}px`,
                    height: `${size}px`,
                    borderRadius: `${size / 2}px`,
                }}
            />)
    )
}

export default Avatar