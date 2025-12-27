import React from 'react'

const Avatar = ({ url, size = 50, children }) => {

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
                >
                    {children}
                </div>
            )
            :
            (<div className="friendTileIcon"
                style={{
                    width: `${size}px`,
                    height: `${size}px`,
                    borderRadius: `${size / 2}px`,
                }}
            >
                {children}
            </div>)
    )
}

export default Avatar