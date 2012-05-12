using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace MogreFrontEnd
{
    class SlerpNode
    {
        public SceneNode Node { get; private set; }

        private Vector2 _destination;
        private float _destinationAngle;

        private int _slerpCounter;
        private int _angleSlerpCounter;
        private Vector2 _currentPosition;
        private Radian _currentAngle;
        private Vector2 _perStep;
        private Radian _perStepAngle;

        private const int SlerpCount = 15;
        private const int AngleSlerpCount = 20;
        //private const int SlerpCount = 1;
        //private const int AngleSlerpCount = 1;

        internal SlerpNode(SceneNode node, Vector2 position, float angle)
        {
            Node = node;

            _currentPosition = position;
            _currentAngle = angle;
        }

        public void Update(Vector2 newDestination, float newAngle)
        {
            UpdatePosition(newDestination);
            UpdateAngle(newAngle);
        }

        private void UpdatePosition(Vector2 newDestination)
        {
            if (_destination != newDestination)
            {
                // Start new slerp
                _destination = newDestination;
                _perStep = (_destination - _currentPosition) / SlerpCount;
                _slerpCounter = 0;
            }
            else
            {
                if (_slerpCounter >= SlerpCount)
                    return;

                _slerpCounter++;
                _currentPosition += _perStep;
            }

            // Update node
            Node.SetPosition(_currentPosition.x, 0, _currentPosition.y);
        }

        private void UpdateAngle(float newAngle)
        {
            if (_destinationAngle != newAngle)
            {
                _destinationAngle = newAngle;
                _perStepAngle = (_destinationAngle - _currentAngle) / AngleSlerpCount;
                _angleSlerpCounter = 0;
            }
            else
            {
                if (_angleSlerpCounter >= AngleSlerpCount)
                    return;

                _angleSlerpCounter++;
                _currentAngle += _perStepAngle;
            }

            // Update node
            Node.ResetOrientation();
            Node.Yaw(-_currentAngle);
        }
    }
}
