import cv2
import mediapipe as mp
import urllib.request
import numpy as np
import socket

HOST = 'localhost'
PORT = 65432

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_hands = mp.solutions.hands

msg = ""
url = 'http://26.159.42.140:8080/shot.jpg'
# url = 'http://1.4:8080/shot.jpg'

with mp_hands.Hands(
    model_complexity=0,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5) as hands, socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:

    # start server
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.bind((HOST, PORT))
    s.listen()
    conn, addr = s.accept()
    print(f'Connected to {addr}')

    while True:
        # Try getting frames from url
        try:
            imgResponse = urllib.request.urlopen(url)
            imgNp = np.array(bytearray(imgResponse.read()),dtype=np.uint8)
            image = cv2.imdecode(imgNp, -1)
        except:
            print('Image not found')
            break

        # To improve performance, optionally mark the image as not writeable to
        # pass by reference.
        image.flags.writeable = False
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        results = hands.process(image)

        # Draw the hand annotations on the image.
        image.flags.writeable = True
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

        # If Hands present
        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:

                # Send coordinates to client
                msg = ""
                for i in range(len(hand_landmarks.landmark)):
                    hx = round(hand_landmarks.landmark[i].x, 5)
                    hy = round(hand_landmarks.landmark[i].y, 5)
                    hz = round(hand_landmarks.landmark[i].z, 5)

                    hl = str(hx) + ',' +  str(hy) + ',' + str(hz) + ','
                    msg += hl
                conn.sendall(str(msg).encode())

                # Draw landmarks
                mp_drawing.draw_landmarks(
                    image,
                    hand_landmarks,
                    mp_hands.HAND_CONNECTIONS,
                    mp_drawing_styles.get_default_hand_landmarks_style())
                    # mp_drawing_styles.get_default_hand_connections_style())

        # Flip the image horizontally for a selfie-view display.
        cv2.imshow('MediaPipe Hands', cv2.flip(image, 1))

        # Break
        if cv2.waitKey(5) & 0xFF == ord('q'):
            break
