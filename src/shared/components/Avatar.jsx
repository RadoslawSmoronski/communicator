import React from 'react'

const Avatar = ({ url, size, children, className = "" }) => {

    const combinedClasses = `friendTileIcon ${className}`.trim();

    return (
        url != null ?
            (
                <div className={combinedClasses}
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
            (<div className={combinedClasses}
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