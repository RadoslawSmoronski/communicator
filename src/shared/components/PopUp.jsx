import React, { useImperativeHandle, forwardRef, useState, useEffect } from 'react';

const PopUp = forwardRef((props, ref) => {
  const [visible, setVisible] = useState(false);
  const [message, setMessage] = useState('');



  useImperativeHandle(ref, () => ({
    show(msg) {
      setMessage(msg);
      setVisible(true);
    },
    hide() {
      setVisible(false);
    }
  }));


  return (
    <>
      {visible && (
        <table className="popUp">
          <tbody>
            <tr>
              <th>{message}</th>
              <th className="closeBtnBox">
                <div className="closeBtn" onClick={() => setVisible(false)} />
              </th>
            </tr>
          </tbody>
        </table>
      )}
    </>
  );
});

export default PopUp;
